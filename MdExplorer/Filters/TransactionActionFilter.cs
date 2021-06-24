//using Ad.Tools.Dal.Abstractions.Interfaces;
//using Microsoft.AspNetCore.Mvc.Filters;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace MdExplorer.Service.Filters
//{
//    public class TransactionActionFilter : IActionFilter
//    {
//        private readonly IServiceProvider _serviceProvider;
//        private readonly IDALFactory _dalFactory;

//        public TransactionActionFilter(IServiceProvider serviceProvider,IDALFactory dalFactory)
//        {
//            _serviceProvider = serviceProvider;
//            _dalFactory = dalFactory;
//        }
        

//        public void OnActionExecuting(ActionExecutingContext context)
//        {            
//            _dalFactory.OpenSession();
            
//        }

//        public void OnActionExecuted(ActionExecutedContext context)
//        {
//            if (context.Exception != null)
//            {
//                _dalFactory.CommitTransaction();
//            }
//            //_dalFactory.CommitTransaction();
//            //_dalFactory.CloseSession();
//            // nothing to do
//        }

        
//    }
//}
