﻿using System.Collections.Generic;
using System.Text;
using Microsoft.R.Support.Help.Definitions;

namespace Microsoft.R.Support.Help.Functions
{
    public sealed class SignatureInfo: ISignatureInfo
    {
        #region ISignatureInfo
        /// <summary>
        /// Function arguments
        /// </summary>
        public IReadOnlyList<IArgumentInfo> Arguments { get; internal set; }

        public string GetSignatureString(string functionName, List<int> locusPoints = null)
        {
            var sb = new StringBuilder(functionName);

            sb.Append('(');

            if (locusPoints != null)
                locusPoints.Add(sb.Length);

            for (int i = 0; i < Arguments.Count; i++)
            {
                IArgumentInfo arg = Arguments[i];
                sb.Append(arg.Name);

                if (!string.IsNullOrEmpty(arg.DefaultValue))
                {
                    sb.Append(" = ");
                    sb.Append(arg.DefaultValue);

                }

                if (i < Arguments.Count - 1)
                {
                    sb.Append(", ");
                }

                if (locusPoints != null)
                    locusPoints.Add(sb.Length);
            }

            sb.Append(')');
            return sb.ToString();
        }
        #endregion
    }
}