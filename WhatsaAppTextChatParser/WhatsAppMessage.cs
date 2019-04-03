using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhatsaAppTextChatParser
{
    class WhatsAppMessage
    {
        public string Text { get; set; }
        public string User { get; set; }
        public DateTime Timestamp { get; set; }
        public string GroupName { get; set; }

        public WhatsAppMessage()
        {
        }

        private static bool GroupChatIsAlreadyParsed(string groupName)
        {
            MySqlInteractions sql = new MySqlInteractions();
            string query = "SELECT count(*) FROM message WHERE group_name = @GROUP_NAME; ";
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            MySqlParameter parGroupName = new MySqlParameter("@GROUP_NAME", MySqlDbType.VarChar, 255);
            parGroupName.Value = groupName;
            parameters.Add(parGroupName);
            List<object[]> results = sql.Select(query, 1, parameters.ToArray());

            if (results.Count == 0) return false;
            long rowCount = (long)results.First()[0];

            return (rowCount > 0);
        }

        public static int SendDataToMySql(List<WhatsAppMessage> messages, int currentBatchIndex)
        {
            int results = -1;

            if (GroupChatIsAlreadyParsed(messages[0].GroupName) && currentBatchIndex == 0)
            {
                UserTextFeedback.ConsoleOut("Insert failed! Group chat already parsed and inserted into MySQL for group name \"" + messages[0].GroupName + "\"");
                return results;
            }

            string query = "INSERT INTO message (timestamp, user, text, group_name) VALUES ";
            List<MySqlParameter> parameters = new List<MySqlParameter>();

            for (int i = 0; i < messages.Count; i++)
            {
                query += "(@timestamp" + i + ",@user" + i + ",@text" + i + ",@group_name" + i + "),";

                MySqlParameter parTimestamp = new MySqlParameter("@timestamp" + i, MySqlDbType.DateTime);
                MySqlParameter parUser = new MySqlParameter("@user" + i, MySqlDbType.VarChar, 255);
                MySqlParameter parText = new MySqlParameter("@text" + i, MySqlDbType.TinyText);
                MySqlParameter parGroupName = new MySqlParameter("@group_name" + i, MySqlDbType.VarChar, 255);

                parTimestamp.Value = messages[i].Timestamp;
                parUser.Value = messages[i].User;
                parText.Value = messages[i].Text;
                parGroupName.Value = messages[i].GroupName;

                parameters.Add(parTimestamp);
                parameters.Add(parUser);
                parameters.Add(parText);
                parameters.Add(parGroupName);
            }
            query = query.TrimEnd(",".ToCharArray());

            try
            {
                MySqlInteractions mySql = new MySqlInteractions();
                results = mySql.Insert(query, parameters.ToArray());
                UserTextFeedback.ConsoleOut(results + " message(s) inserted.");
            }
            catch (Exception ex)
            {
                UserTextFeedback.ConsoleOut(ex.ToString());
            }

            return results;
        }
    }
}
