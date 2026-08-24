using System.Runtime.CompilerServices;
namespace SanSo.Migrator.V3;
internal static class LegacyProjectionCompilerCompatibility
{
 [ModuleInitializer]internal static void ConfigureLegacyPlpgsqlCompiler(){var value=Environment.GetEnvironmentVariable("SANSO_POSTGRES");if(!string.IsNullOrWhiteSpace(value)&&!value.Contains("Options=",StringComparison.OrdinalIgnoreCase))Environment.SetEnvironmentVariable("SANSO_POSTGRES",value+";Options=-c plpgsql.variable_conflict=use_column");}
}
