using GitControl.View.Dialogs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitControl.Messages
{
    public enum Data
    {
        UserProfile,
        Repositories,
        DialogTypeControl,
    }
    class DataRequestMessage
    {
        public DataRequestMessage(object? identity, Data type) // identity - who is requesting
        {
            Type = type;
            Identity = identity;
        }
        public Data Type { get; set; }
        public object? Identity { get; }
        
        private bool _requestReceived {get;set;} = false;
        public bool requestReceived { get { return _requestReceived; } }
        public bool MarkAsReceived()
        {
            bool success = false;
            lock (this)
            {
                if (requestReceived == false)
                {
                    _requestReceived = true;
                    success = true;
                }
            }
            return success;
        }
        private bool _responseCreated {  get; set; } = false;
        public bool ResponseCreated { get { return _responseCreated; } }

        public DataResponseMessage responseToMessage(object Data)
        {
            _responseCreated = true;
            return new DataResponseMessage(this, Data);
        }
    }
    class DataResponseMessage
    {
        public DataResponseMessage(DataRequestMessage request, object data) // identity - Response to request identity
        {
            Request = request;
            Data = data;
        }
        public DataRequestMessage Request { get; set; }
        public Data Type { get { return Request.Type; } }
        public object? Identity { get { return Request.Identity; } }
        public object Data { get; set; }

    }
    class DataUpdateMessage
    {
        public DataUpdateMessage(Data type, object data)
        {
            Type = type;
            Data = data;
        }
        public Data Type { get; set; }
        public object Data { get; set; }
        public bool _updatedMsgReceived { get; set; } = false;
    }
    class DataUpdatedMessage
    {
        public DataUpdatedMessage(Data type, object data)
        {
            Type = type;
            Data = data;
        }
        public Data Type { get; set; }
        public object? Data { get; set; }
    }
}