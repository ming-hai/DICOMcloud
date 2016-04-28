﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DICOMcloud.Dicom.Media;
using DICOMcloud.Core.Storage;

namespace DICOMcloud.Wado.Models
{
    public class WadoResponse : IWadoRsResponse
    {
        public WadoResponse () {} 
        public WadoResponse ( IStorageLocation location, string mimeType )
        {
            StorageLocation = location ; 
        
            Content       = StorageLocation.GetReadStream ( ) ;
            ContentLength = Content.Length ;    
            MimeType      = mimeType ;
        }
         
      public Stream Content
      {
         get;
         set;
      }

        public long ContentLength
        {
            get ;

            set ;
        }

        public MimeMediaType MimeType
      {
         get;
         set;
      }
        public IStorageLocation StorageLocation { get; private set; }
        public string TransferSyntax { get; set; }
   }
}
