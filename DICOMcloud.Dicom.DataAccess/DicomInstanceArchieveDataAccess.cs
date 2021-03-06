﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DICOMcloud.Core.Extensions ;
using DICOMcloud.Dicom.DataAccess.Matching;
using DICOMcloud.Dicom.DataAccess.DB;
using System.Data.SqlClient;
using DICOMcloud.Dicom.DataAccess.DB.Schema;
using System.Data;
using DICOMcloud.Dicom.Data;

namespace DICOMcloud.Dicom.DataAccess
{
    public interface IDicomInstanceArchieveDataAccess : IDicomInstnaceStorageDataAccess, IDicomStorageQueryDataAccess
    {}

    public class DicomInstanceArchieveDataAccess : IDicomInstanceArchieveDataAccess
    {
        public string ConnectionString { get; set ; }

        public DicomInstanceArchieveDataAccess() : this("") { 
        
        }
        public DicomInstanceArchieveDataAccess ( string connectionString )
        { 
            ConnectionString = connectionString ;
        }

        public virtual void Search
        (
            IEnumerable<IMatchingCondition> conditions, 
            IStorageDataReader responseBuilder,
            IQueryOptions options,
            string queryLevel
        )
        {
            string[]         tables ;
            DicomDataAdapter dbAdapter ;
            IDbCommand       cmd ;


            dbAdapter = CreateDataAdapter ( ) ;
            cmd       = dbAdapter.CreateSelectCommand ( queryLevel, conditions, options, out tables );

            cmd.Connection.Open ( );

            try
            {
                List<IDicomDataParameter> parameters = new List<IDicomDataParameter> ( );

                using ( var reader = cmd.ExecuteReader ( ) )
                {
                    int currentTableIndex = -1;


                    do
                    {
                        currentTableIndex++;

                        responseBuilder.BeginResultSet ( tables[currentTableIndex] );

                        while ( reader.Read ( ) )
                        {
                            responseBuilder.BeginRead ( );

                            for ( int columnIndex = 0; columnIndex < reader.FieldCount; columnIndex++ )
                            {
                                string columnName = reader.GetName ( columnIndex );
                                string tableName = tables[currentTableIndex];

                                object value = reader.GetValue ( columnIndex );

                                responseBuilder.ReadData ( tableName, columnName, value );
                            }

                            responseBuilder.EndRead ( );
                        }

                        responseBuilder.EndResultSet ( );

                    } while ( reader.NextResult ( ) );
                }
            }
            finally
            {
                if ( cmd.Connection.State == System.Data.ConnectionState.Open )
                {
                    cmd.Connection.Close ( );
                }
            }
        }

        public virtual void StoreInstance 
        ( 
            IObjectID objectId,  
            IEnumerable<IDicomDataParameter> parameters, 
            InstanceMetadata data = null
        )
        {
            //TODO: use transation
            //dbAdapter.CreateTransation ( ) 

            var cmd = CreateDataAdapter ( ).CreateInsertCommand ( parameters, data );

            cmd.Connection.Open ( );

            try
            {
                int rowsInserted = cmd.ExecuteNonQuery ( );

                if ( rowsInserted <= 0 )
                {
                    //return duplicate instance?!!!
                }

                if ( null != data )
                {
                    StoreInstanceMetadata ( objectId, data );
                }
            }
            finally
            {
                cmd.Connection.Close ( );
            }
        }

        public virtual void StoreInstanceMetadata ( IObjectID objectId, InstanceMetadata data )
        {
            StoreInstanceMetadata ( objectId, data, CreateDataAdapter ( ) );
        }

        public virtual InstanceMetadata GetInstanceMetadata( IObjectID instance ) 
        {
            DicomDataAdapter dbAdapter = CreateDataAdapter ( ) ;
            
            
            var cmd = dbAdapter.CreateGetMetadataCommand ( instance ) ;
        
            cmd.Connection.Open ( ) ;

            try
            {
                InstanceMetadata metadata      = null ;
                object           metadataValue = cmd.ExecuteScalar     ( ) ;
                

                if ( null != metadataValue && DBNull.Value != metadataValue )
                {   
                    string metaDataString = (string) metadataValue ;


                    metadata = metaDataString.FromJson<InstanceMetadata> ( ) ;
                }

                return metadata ;
            }
            finally
            { 
                cmd.Connection.Close ( ) ;
            }
        }

        public virtual void DeleteInstance ( string instance )
        {
            DicomDataAdapter dbAdapter = CreateDataAdapter ( ) ;
            IDbCommand cmd ;
            
            dbAdapter.CreateConnection ( ) ;
            
            cmd = dbAdapter.CreateDeleteInstanceCommand ( instance ) ;
        
            cmd.Connection.Open ( ) ;

            try
            { 
                cmd.ExecuteScalar ( ) ;
            }
            finally
            { 
                cmd.Connection.Close ( ) ;
            }
        }

        protected virtual void StoreInstanceMetadata 
        ( 
            IObjectID objectId,
            InstanceMetadata data, 
            DicomDataAdapter dbAdapter 
        )
        {
            //TODO: use transaction
            //dbAdapter.CreateTransaction ( ) 

            var cmd = dbAdapter.CreateUpdateMetadataCommand ( objectId, data );

            cmd.Connection.Open ( );

            try
            {
                int rowsInserted = cmd.ExecuteNonQuery ( );

                if ( rowsInserted <= 0 )
                {
                    //TODO: return duplicate instance?!!!
                }
            }
            finally
            {
                cmd.Connection.Close ( );
            }
        }

        protected virtual DicomDataAdapter CreateDataAdapter ( )
        {
            return new DicomSqlDataAdapter ( ConnectionString ) ;
        }

    }
}
