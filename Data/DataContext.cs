using Microsoft.EntityFrameworkCore;
using SLA_Management.Models;

namespace SLAManagement.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

       
    }
}
