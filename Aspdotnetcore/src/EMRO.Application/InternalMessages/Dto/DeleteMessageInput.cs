using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EMRO.InternalMessages.Dto
{
   public class DeleteMessageInput
    {
        public string Id { get; set; }
        public bool IsDeleteReceiver  { get; set; }
        public bool IsDeleteSender  { get; set; }
        public bool IsDeleteTrash  { get; set; }
        public bool IsDeleteDraft  { get; set; }

        [Required]
        public Guid UserId { get; set; }
    }
}
