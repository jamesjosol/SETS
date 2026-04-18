using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Main;

namespace Service.Interfaces
{
    public interface IHealthService
    {
        HealthCheckResult CheckDatabase();
        HealthCheckResult CheckHcLab();
    }
}