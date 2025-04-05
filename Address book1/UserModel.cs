using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Address_book1
{
    public class UserModel
    {
        public int user_id {  get; set; }
        public string username {  get; set; }
        public string password_hash {  get; set; }
        public string role {  get; set; }
    }
}
