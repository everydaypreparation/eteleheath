using Abp.Application.Services;
using Abp.Domain.Repositories;
using EMRO.Appointment;
using EMRO.AppointmentSlot;
using EMRO.Common.Samvaad.Dto;
using EMRO.MeetingLogs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Xml;

namespace EMRO.Common.Samvaad
{
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class JoinMeeting : ApplicationService, IJoinMeeting
    {
        private readonly SamvaadParams _samvaadParams;
        private static string _queryString;
        private static string _apiUrl;
        private readonly IDoctorAppointmentRepository _doctorAppointmentRepository;
        private readonly IRepository<MeetingLogDetails, Guid> _meetingLogDetails;
        private readonly IRepository<DoctorAppointmentSlot, Guid> _doctorAppointmentSlotRepository;

        public JoinMeeting(IOptions<SamvaadParams> samvaadParams,
            IRepository<MeetingLogDetails, Guid> meetingLogDetails,
            IRepository<DoctorAppointmentSlot, Guid> doctorAppointmentSlotRepository,
            IDoctorAppointmentRepository doctorAppointmentRepository)
        {
            _doctorAppointmentRepository = doctorAppointmentRepository;
            _doctorAppointmentSlotRepository = doctorAppointmentSlotRepository;
            _samvaadParams = samvaadParams.Value;
            _meetingLogDetails = meetingLogDetails;
        }
        private string CallAPI(string method, Dictionary<string, string> parameter)
        {
            var query = HttpUtility.ParseQueryString("");
            foreach (KeyValuePair<string, string> entry in parameter)
            {
                query[entry.Key] = entry.Value;
            }

            _queryString = method + query.ToString() + _samvaadParams.API_SECRET.ToString();

            var enc = Encoding.GetEncoding(0);
            byte[] buffer = enc.GetBytes(_queryString);
            var sha1 = SHA1.Create();
            var checksum = BitConverter.ToString(sha1.ComputeHash(buffer)).Replace("-", "");

            _apiUrl = _samvaadParams.API_URL.ToString() + method + "?" + query.ToString() + "&checksum=" + checksum.ToLower();

            if (_apiUrl.Contains("join"))
            {
                return _apiUrl;
            }

            HttpClientHandler httpMessageHandler = new HttpClientHandler();
            httpMessageHandler.ServerCertificateCustomValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            using (var client = new HttpClient(httpMessageHandler))
            {
                try
                {
                    var httpResponseMessage = client.GetAsync(_apiUrl);
                    if (httpResponseMessage.Result.StatusCode == HttpStatusCode.OK)
                    {
                        // Do something...
                        var response = httpResponseMessage.Result.Content.ReadAsStringAsync().Result;
                        Logger.Info(response.ToString());  // response here...

                        return response.ToString();
                    }
                    else
                    {
                        Logger.Error("Some error occured in joining meeting." + httpResponseMessage.Result.Content.ToString());
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.ToString()); // error here...
                }
            }

            //comment this
            return "";
        }
        private string CreateMeeting(string meetingID)
        {
            Dictionary<string, string> parameter = new Dictionary<string, string>();

            parameter.Add("allowStartStopRecording", _samvaadParams.samvaadMeetingParams.allowStartStopRecording.ToString());
            parameter.Add("attendeePW", _samvaadParams.ATTENDEE_PASS);
            parameter.Add("autoStartRecording", _samvaadParams.samvaadMeetingParams.autoStartRecording.ToString());
            parameter.Add("meetingID", meetingID);
            parameter.Add("moderatorPW", _samvaadParams.MODERATOR_PASS);
            parameter.Add("name", _samvaadParams.samvaadMeetingParams.name);
            parameter.Add("record", _samvaadParams.samvaadMeetingParams.record.ToString());
            parameter.Add("welcome", _samvaadParams.samvaadMeetingParams.welcome);
            parameter.Add("meta_DisableChat", _samvaadParams.samvaadMeetingParams.meta_DisableChat.ToString().ToLower());
            parameter.Add("meta_DisableUsers", _samvaadParams.samvaadMeetingParams.meta_DisableUsers.ToString().ToLower());
            parameter.Add("meta_DisableNotes", _samvaadParams.samvaadMeetingParams.meta_DisableNotes.ToString().ToLower());
            parameter.Add("meta_DisablePoll", _samvaadParams.samvaadMeetingParams.meta_DisablePoll.ToString().ToLower());
            parameter.Add("meta_DisableSetting", _samvaadParams.samvaadMeetingParams.meta_DisableSetting.ToString().ToLower());
            parameter.Add("meta_DisablePresentation", _samvaadParams.samvaadMeetingParams.meta_DisablePresentation.ToString().ToLower());
            parameter.Add("meta_DisablePoweredBy", _samvaadParams.samvaadMeetingParams.meta_DisablePoweredBy.ToString().ToLower());
            parameter.Add("meta_DisableHeader", _samvaadParams.samvaadMeetingParams.meta_DisableHeader.ToString().ToLower());
            parameter.Add("meta_DisableExternalVideo", _samvaadParams.samvaadMeetingParams.meta_DisableExternalVideo.ToString().ToLower());
            parameter.Add("meta_FooterStyle", _samvaadParams.samvaadMeetingParams.meta_FooterStyle);
            // parameter.Add("logoutURL", _samvaadParams.samvaadMeetingParams.logoutURL);
            parameter.Add("meta_DisableGoLive", _samvaadParams.samvaadMeetingParams.meta_DisableGoLive.ToString().ToLower());
            // parameter.Add("meta_DisableScreenShare", _samvaadParams.samvaadMeetingParams.meta_DisableScreenShare);
            parameter.Add("meta_textColorPrimary", _samvaadParams.samvaadMeetingParams.meta_textColorPrimary);

            parameter.Add("logoutURL", _samvaadParams.samvaadMeetingParams.logoutURL);
            parameter.Add("meta_DisableScreenShare", _samvaadParams.samvaadMeetingParams.meta_DisableScreenShare.ToString().ToLower());
            parameter.Add("meta_DisableUserSelection", _samvaadParams.samvaadMeetingParams.meta_DisableUserSelection.ToString().ToLower());
            parameter.Add("meta_MiniFooterColor", _samvaadParams.samvaadMeetingParams.meta_MiniFooterColor);
            parameter.Add("meta_VideoStyle", _samvaadParams.samvaadMeetingParams.meta_VideoStyle);

            string meetingURL = CallAPI("create", parameter);
            return meetingURL;
        }
        private string JoinRunningMeeting(string fullName, string meetingID)
        {
            Dictionary<string, string> parameter = new Dictionary<string, string>();

            parameter.Add("fullName", fullName);
            parameter.Add("meetingID", meetingID);
            parameter.Add("password", _samvaadParams.MODERATOR_PASS);
            parameter.Add("redirect", _samvaadParams.samvaadMeetingParams.redirect.ToString());

            string meetingURL = CallAPI("join", parameter);
            return meetingURL;
        }

        [HttpGet]
        public string JoinSamvaadMeeting(string meetingID, string fullName)
        {
            var appointment = _doctorAppointmentRepository.GetAllList(x => x.meetingId == meetingID);
            if (appointment.Count > 0)
            {
                var slot = _doctorAppointmentSlotRepository.GetAllList(x => x.Id == appointment[0].AppointmentSlotId);
                if (slot[0].SlotZoneTime > DateTime.UtcNow.AddMinutes(10))
                {
                    return "Meeting isn't started yet.";
                }
                if (slot[0].SlotZoneEndTime <= DateTime.UtcNow)
                {
                    return "Meeting Over";
                }
            }
            else
            {
                var meetingLog = _meetingLogDetails.GetAllList(x => x.MeetingId == new Guid(meetingID));
                if (meetingLog.Count > 0)
                {
                    if (meetingLog[0].CreatedOn.Value.AddHours(2) <= DateTime.UtcNow)
                    {
                        return "Meeting Over";
                    }
                }
            }

            Dictionary<string, string> parameter = new Dictionary<string, string>();
            parameter.Add("meetingID", meetingID);

            string output = CallAPI("isMeetingRunning", parameter);

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(output);

            var isRunning = xmlDocument.GetElementsByTagName("running");

            if (isRunning[0].InnerText == "false")
            {
                CreateMeeting(meetingID);
            }

            string joinurl = JoinRunningMeeting(fullName, meetingID);

            return joinurl;
        }
    }
}
