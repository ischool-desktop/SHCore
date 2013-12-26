using System;
using FISCA.DSAUtil;

namespace SmartSchool
{
    public class SendRequestException : Exception
    {
        private string _service_name;
        private DSRequest _request;

        public SendRequestException(string serviceName, DSRequest request, Exception innerException)
            : base("�I�s�u" + serviceName + "�v�A�ȿ��~�C", innerException)
        {
            _service_name = serviceName;
            _request = request;
        }

        public string ServiceName
        {
            get { return _service_name; }
        }

        public DSRequest Request
        {
            get { return _request; }
        }

    }
}
