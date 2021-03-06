﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using fo = Dicom;
using Dicom.Imaging ;
using DICOMcloud.Core.Storage;


namespace DICOMcloud.Dicom.Media
{
    public abstract class DicomMediaWriterBase : IDicomMediaWriter
    {
        public ILocationProvider MediaStorage { get; set; }

        public DicomMediaWriterBase() : this(new FileStorageService())
        { }

        public DicomMediaWriterBase ( ILocationProvider storageProvider )
        {
            MediaStorage = storageProvider ;
        }

        public abstract string MediaType
        {
            get ;
        }

        public IList<IStorageLocation> CreateMedia
        (
            DicomMediaWriterParameters mediaParameters
        )
        {
            return CreateMedia ( mediaParameters, MediaStorage ) ;
        }

        public IList<IStorageLocation> CreateMedia
        (
            DicomMediaWriterParameters mediaParameters, 
            ILocationProvider sotrageProvider 
        )
        {
            if (null != sotrageProvider )
            {
                int                    framesCount    = 1;
                List<IStorageLocation> locations      = new List<IStorageLocation> ( ) ;
                var                    dataset        = GetMediaDataset ( mediaParameters.Dataset, mediaParameters.MediaInfo ) ;
                string                 transferSyntax = ( !string.IsNullOrWhiteSpace (mediaParameters.MediaInfo.TransferSyntax ) ) ? ( mediaParameters.MediaInfo.TransferSyntax ) : "" ;

                if ( StoreMultiFrames )
                {
                    DicomPixelData pd ;


                    pd          = DicomPixelData.Create ( mediaParameters.Dataset ) ;
                    framesCount = pd.NumberOfFrames ;
                }
                
                for ( int frame = 1; frame <= framesCount; frame++ )
                {
                    var storeLocation = sotrageProvider.GetLocation ( new DicomMediaId ( mediaParameters.Dataset, frame, MediaType, transferSyntax ));
                    
                    
                    Upload ( dataset, frame, storeLocation ) ;
                
                    locations.Add ( storeLocation ) ;
                }

                return locations ;
            }

            throw new InvalidOperationException ( "No MediaStorage service found") ;
        }


        public IList<IStorageLocation> CreateMedia
        (
            DicomMediaWriterParameters mediaParameters, 
            ILocationProvider storageProvider,
            int[] frameList
        )
        {
            if ( null == storageProvider ) { throw new InvalidOperationException ( "No MediaStorage service found") ; }

            List<IStorageLocation> locations      = new List<IStorageLocation> ( ) ;
            var                    dataset        = GetMediaDataset ( mediaParameters.Dataset, mediaParameters.MediaInfo ) ;
            string                 transferSyntax = ( !string.IsNullOrWhiteSpace (mediaParameters.MediaInfo.TransferSyntax ) ) ? ( mediaParameters.MediaInfo.TransferSyntax ) : "" ;


            if ( !StoreMultiFrames )
            {
                throw new InvalidOperationException ( "Media writer doesn't support generating frames" ) ;
            }

            foreach ( int frame in frameList )
            {
                var storeLocation = storageProvider.GetLocation ( new DicomMediaId ( mediaParameters.Dataset, frame, MediaType, transferSyntax ));
                    
                    
                Upload ( mediaParameters.Dataset, frame, storeLocation ) ;
                
                locations.Add ( storeLocation ) ;
            }

            return locations ;
        
        }

        protected virtual fo.DicomDataset GetMediaDataset ( fo.DicomDataset data, DicomMediaProperties mediaInfo )
        {
            return data ;
        }

        protected abstract bool StoreMultiFrames { get; }

        protected abstract void Upload(fo.DicomDataset dataset, int frame, IStorageLocation storeLocation);
    }
}
