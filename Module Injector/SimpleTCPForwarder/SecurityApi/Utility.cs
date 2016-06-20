using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExploitFilter.SecurityApi
{
    public class Utility
    {
        public static string HexDump(byte[] buffer)
        {
            return HexDump(buffer, 0, buffer.Length);
        }

        public static string HexDump(byte[] buffer, int offset, int count)
        {
            StringBuilder builder = new StringBuilder();
            StringBuilder builder2 = new StringBuilder();
            int num = count;
            if ((num % 0x10) != 0)
            {
                num += 0x10 - (num % 0x10);
            }
            for (int i = 0; i <= num; i++)
            {
                if ((i % 0x10) == 0)
                {
                    if (i > 0)
                    {
                        builder.AppendFormat("  {0}{1}", builder2.ToString(), Environment.NewLine);
                        builder2.Clear();
                    }
                    if (i != num)
                    {
                        builder.AppendFormat("{0:d10}   ", i);
                    }
                }
                if (i < count)
                {
                    builder.AppendFormat("{0:X2} ", buffer[offset + i]);
                    char c = (char)buffer[offset + i];
                    if (!char.IsControl(c))
                    {
                        builder2.AppendFormat("{0}", c);
                    }
                    else
                    {
                        builder2.Append(".");
                    }
                }
                else
                {
                    builder.Append("   ");
                    builder2.Append(".");
                }
            }
            return builder.ToString();
        }
    }
}
