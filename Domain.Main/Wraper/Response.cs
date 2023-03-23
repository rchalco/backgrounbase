using System;
using System.Collections;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace Domain.Main.Wraper
{
    #region Enumeradores

    [DataContract]
    public enum ResponseType
    {
        [EnumMember(Value = "Success")]
        Success = 1,

        [EnumMember(Value = "Warning")]
        Warning = 2,

        [EnumMember(Value = "Error")]
        Error = 3,

        [EnumMember(Value = "NoData")]
        NoData = 4
    }

    
    [DataContract]
    [Serializable]
    public class Response
    {
        [DataMember]
        public ResponseType State { get; set; }

        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public string Code { get; set; }
    }

    [DataContract]
    [Serializable]
    public class ResponseOperation : Response
    {
        public string Trace { get; set; }
    }

    [DataContract(IsReference = true)]
    [Serializable]
    public class ResponseQuery : Response
    {
        [DataMember]
        public IList ListEntities { get; set; }
    }


    [DataContract]
    [Serializable]
    public class ResponseQuery<T> : Response where T : class
    {
        [DataMember]
        public List<T> ListEntities { get; set; }
    }

    
    [DataContract]
    [Serializable]
    public class ResponseObject<T> : Response
    {
        [DataMember]
        public string Code { get; set; }

        [DataMember]
        public T Object { get; set; }
    }

    [DataContract(IsReference = true)]
    [Serializable]
    public class ResponseObject : Response
    {
        [DataMember]
        public string Code { get; set; }

        [DataMember]
        public Object Object { get; set; }
    }

    #endregion Enumeradores
}