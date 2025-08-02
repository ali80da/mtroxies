using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roxi.Core.Models.V01.Common;

namespace Roxi.Core.Services.V01.Robot
{
    public interface ITeleRobotService
    {

        Task<ResultConditions<bool>> RegisterProxyAsync(int port, string secret, string sponsorChannel);


    }





}