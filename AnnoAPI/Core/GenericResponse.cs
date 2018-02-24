
namespace AnnoAPI.Core
{
    public class GenericResponse
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }

        public GenericResponse() { }
        public GenericResponse(object data, string code, string message)
        {
            Code = code;
            Message = message;
            Data = data;
        }
    }
}