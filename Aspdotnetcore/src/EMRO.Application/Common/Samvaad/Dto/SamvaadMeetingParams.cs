using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Common.Samvaad.Dto
{
    public class SamvaadMeetingParams
    {
        public bool allowStartStopRecording { get; set; }
        public bool autoStartRecording { get; set; }
        public string name { get; set; }
        public bool record { get; set; }
        public string welcome { get; set; }
        public bool redirect { get; set; }
        public bool meta_DisableChat { get; set; }
        public bool meta_DisableUsers { get; set; }
        public bool meta_DisableNotes { get; set; }
        public bool meta_DisablePoll { get; set; }
        public bool meta_DisableSetting { get; set; }
        public bool meta_DisablePresentation { get; set; }
        public bool meta_DisablePoweredBy { get; set; }
        public bool meta_DisableHeader { get; set; }
        public bool meta_DisableExternalVideo { get; set; }
        public string meta_FooterStyle { get; set; }
        public bool meta_DisableGoLive { get; set; }
        public bool meta_DisableScreenShare { get; set; }
        public bool meta_DisableUserSelection { get; set; }
        public string meta_MiniFooterColor { get; set; }
        public string meta_VideoStyle { get; set; }
        public string meta_textColorPrimary { get; set; }
        public string logoutURL { get; set; }
    }
}
