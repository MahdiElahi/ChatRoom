using DomainClasses.User;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace DomainClasses.Chat
{
    public class ChatMessage
    {
        public ChatMessage()
        {

        }
        [Key]

        public int Chat_Id { get; set; }
        public DateTime Date { get; set; }
        public string Message { get; set; }
        public bool Type { get; set; }
        public ChatGroup ChatGroup { get; set; }
        public int Group_Id { get; set; }
        [ForeignKey("UserId_1")]
        public ApplicationUser ApplicationUser_UserId1 { get; set; }

        public string UserId_1 { get; set; }

        [ForeignKey("UserId_2")]
        public ApplicationUser ApplicationUser_UserId2 { get; set; }

        public string UserId_2 { get; set; }
        public bool IsNew{ get; set; }

    }
}
