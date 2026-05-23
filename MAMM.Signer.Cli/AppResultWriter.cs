using MAMM.Signer.Core;
using System.Text;

namespace MAMM.Signer.Cli;

internal class AppResultWriter(
      AppResultFormatter formatter
    )
{
    public virtual void Write(
          AppResult result
        )
    {
        var buffer = new StringBuilder();
        if(formatter.FormatPreamble( buffer, result ))
        {
            buffer.AppendLine();
            foreach(var r in result.OpResults)
            {
                formatter.Format( buffer, r );
                buffer.AppendLine();
            }
        }
        else buffer.AppendLine();
        Console.WriteLine( buffer.ToString() );
    }
}
