using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EMRO.Documents.Dtos
{
    public class DownloadDocumentInput
    {
        //[Range(1, long.MaxValue)]
        public long Id { get; set; }
    }
}
