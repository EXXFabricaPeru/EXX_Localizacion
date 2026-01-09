using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace exxis_localizacion.entidades
{
    public enum ItemType
    {
        None = 0,
        User_Tables = 1,
        User_Fields = 2,
        User_Objects = 3,
        User_Table_Data = 4,
        User_Object_Data = 5,
        Query_Category = 6,
        User_Query = 7,
        Formatted_Search = 8,
        Script = 9,
        Electronic_File_Format = 10,
        Crystal_Report = 11
    }
}
