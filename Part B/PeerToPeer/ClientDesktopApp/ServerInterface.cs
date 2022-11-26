using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace ClientDesktopApp
{
    [ServiceContract]
    internal interface ServerInterface
    {
        [OperationContract]
        List<String> getJob();

        [OperationContract]
        List<String> getAnwers();

        [OperationContract]
        void UploadAnswers(List<String> answerList, List<String> jobs);


        [OperationContract]
        void UploadJob(String job, String URL, int ID);

    }
}
