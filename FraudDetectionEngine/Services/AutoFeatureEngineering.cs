using Microsoft.ML;
using Microsoft.ML.Data;
public static class AutoFeatureEngineering
{
    public static IEstimator<ITransformer> BuildPipeline(MLContext mlContext, IDataView dataView, string labelColumn, out string[] featureColumns)
    {
        var schema = dataView.Schema;
        var steps = new List<IEstimator<ITransformer>>();

        var numericColumns = new List<string>();
        var categoricalColumns = new List<string>();

        foreach (var column in schema)
        {
            if (column.Name == labelColumn)
                continue; // Skip the label column

            if (column.Type == NumberDataViewType.Single)
                numericColumns.Add(column.Name);
            else if (column.Type == NumberDataViewType.Int32 || column.Type == NumberDataViewType.Int64)
            {
                // Convert int columns to float
                steps.Add(mlContext.Transforms.Conversion.ConvertType(
                    new[] { new InputOutputColumnPair(column.Name) }, outputKind: DataKind.Single));
                numericColumns.Add(column.Name);
            }
            else if (column.Type == BooleanDataViewType.Instance)
            {
                // Convert bools to float 0/1
                steps.Add(mlContext.Transforms.Conversion.ConvertType(
                    new[] { new InputOutputColumnPair(column.Name) }, outputKind: DataKind.Single));
                numericColumns.Add(column.Name);
            }
            else if (column.Type == TextDataViewType.Instance)
            {
                // One-hot encode strings
                steps.Add(mlContext.Transforms.Categorical.OneHotEncoding(
                    new[] { new InputOutputColumnPair(column.Name) }));
                categoricalColumns.Add(column.Name);
            }
        }

        featureColumns = numericColumns.Concat(categoricalColumns).ToArray();

        steps.Add(mlContext.Transforms.Concatenate("Features", featureColumns));
        steps.Add(mlContext.Transforms.NormalizeMinMax("Features")); // Optional normalization

        return steps.Aggregate((prev, next) => prev.Append(next));
    }
}
