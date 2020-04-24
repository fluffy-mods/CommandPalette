using Verse;

namespace CommandPalette
{
    public static class I18n
    {
        private static string Translate( string key, params NamedArgument[] args )
        {
            return Key( key ).Translate( args ).Resolve();
        }

        private static string Key( string key )
        {
            return $"Fluffy.CommandPalette.{key}";
        }
    }
}