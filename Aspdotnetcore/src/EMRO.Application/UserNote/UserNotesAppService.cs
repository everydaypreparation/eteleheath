using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using EMRO.Appointment;
using EMRO.Authorization.Users;
using EMRO.UserNote.Dto;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EMRO.UserNote
{
    [AbpAuthorize]
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class UserNotesAppService : ApplicationService, IUserNotesAppService
    {
        private readonly IRepository<UserNotes, Guid> _userNotesRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly UserManager _userManager;
        private readonly IRepository<DoctorAppointment, Guid> _appoinmentRepository;
        public UserNotesAppService(IRepository<UserNotes, Guid> userNotesRepository, UserManager userManager,
            IRepository<User, long> userRepository
            , IRepository<DoctorAppointment, Guid> appoinmentRepository)
        {
            _userNotesRepository = userNotesRepository;
            _userManager = userManager;
            _userRepository = userRepository;
            _appoinmentRepository = appoinmentRepository;
        }

        public async Task<UserNotesOutput> Create(UserNotesInput input)
        {
            UserNotesOutput output = new UserNotesOutput();
            try
            {
                if (input.UserId != null && input.UserId != Guid.Empty && !string.IsNullOrEmpty(input.Notes))
                {
                    var notes = new UserNotes
                    {
                        UserId = input.UserId,
                        IsActive = true,
                        Notes = input.Notes,
                        CreatedBy = input.UserId.ToString(),
                        CreatedOn = DateTime.UtcNow,
                        TenantId = AbpSession.TenantId
                    };
                    if (input.AppointmentId != null && input.AppointmentId != Guid.Empty)
                    {
                        notes.AppointmentId = input.AppointmentId;
                    }
                    var newid = await _userNotesRepository.InsertAndGetIdAsync(notes);
                    output.NotesId = newid;
                    output.Message = "Notes created successfully.";
                    output.StatusCode = 200;
                }
                else
                {
                    output.Message = "Bad request.";
                    output.StatusCode = 401;
                }

            }
            catch (Exception ex)
            {
                output.Message = "Someting went wrong.";
                output.StatusCode = 500;
                Logger.Error("Create Notes" + ex.StackTrace);
            }
            return output;
        }

        public async Task<UserNotesOutput> CreateUpdateNotesForSamvaad(UserNotesInput input)
        {
            UserNotesOutput output = new UserNotesOutput();

            try
            {
                if (input.UserId != null && input.UserId != Guid.Empty && !string.IsNullOrEmpty(input.Notes) && input.AppointmentId != Guid.Empty && input.AppointmentId != null)
                {
                    var notes = await _userNotesRepository.FirstOrDefaultAsync(x => x.UserId == input.UserId && x.AppointmentId == input.AppointmentId && x.IsActive == true && x.IsSamvaad == true);
                    if (notes != null)
                    {
                        notes.Notes = input.Notes;
                        notes.UpdatedBy = input.UserId.ToString();
                        notes.UpdatedOn = DateTime.UtcNow;

                        if (input.AppointmentId != null && input.AppointmentId != Guid.Empty)
                        {
                            notes.AppointmentId = input.AppointmentId;
                        }

                        await _userNotesRepository.UpdateAsync(notes);

                        output.Message = "Notes updated successfully.";
                        output.StatusCode = 200;
                    }
                    else
                    {
                        var newNotes = new UserNotes
                        {
                            UserId = input.UserId,
                            IsActive = true,
                            Notes = input.Notes,
                            CreatedBy = input.UserId.ToString(),
                            CreatedOn = DateTime.UtcNow,
                            TenantId = AbpSession.TenantId,
                            IsSamvaad = true
                        };
                        if (input.AppointmentId != null && input.AppointmentId != Guid.Empty)
                        {
                            newNotes.AppointmentId = input.AppointmentId;
                        }
                        var newid = await _userNotesRepository.InsertAndGetIdAsync(newNotes);
                        output.NotesId = newid;
                        output.Message = "Notes created successfully.";
                        output.StatusCode = 200;
                    }
                }
                else
                {
                    output.Message = "Bad request.";
                    output.StatusCode = 401;
                }
            }
            catch (Exception ex)
            {
                output.Message = "Someting went wrong.";
                output.StatusCode = 500;
                Logger.Error("Create Notes" + ex.StackTrace);
            }

            return output;
        }

        public async Task<UserNotesOutput> Delete(string NotesId)
        {
            UserNotesOutput output = new UserNotesOutput();
            try
            {
                if (!string.IsNullOrEmpty(NotesId))
                {
                    Guid Id = new Guid(NotesId);
                    var notes = await _userNotesRepository.FirstOrDefaultAsync(x => x.Id == Id && x.IsActive == true);
                    if (notes != null)
                    {
                        notes.IsActive = false;
                        output.Message = " Note Deleted successfully.";
                        output.StatusCode = 200;
                    }
                    else
                    {
                        output.Message = "No record found.";
                        output.StatusCode = 401;
                    }
                }
                else
                {
                    output.Message = "Bad request.";
                    output.StatusCode = 401;
                }

            }
            catch (Exception ex)
            {
                output.Message = "Someting went wrong.";
                output.StatusCode = 500;
                Logger.Error("Get Notes" + ex.StackTrace);
            }
            return output;
        }

        public async Task<UserNotesOutput> Get(string NotesId)
        {
            UserNotesOutput output = new UserNotesOutput();
            try
            {
                if (!string.IsNullOrEmpty(NotesId))
                {
                    Guid Id = new Guid(NotesId);
                    var notes = await _userNotesRepository.FirstOrDefaultAsync(x => x.Id == Id && x.IsActive == true);
                    if (notes != null)
                    {
                        output.NotesId = notes.Id;
                        output.Notes = notes.Notes;
                        output.Message = " Get Note details successfully.";
                        output.StatusCode = 200;
                    }
                    else
                    {
                        output.Message = "No record found.";
                        output.StatusCode = 401;
                    }
                }
                else
                {
                    output.Message = "Bad request.";
                    output.StatusCode = 401;
                }

            }
            catch (Exception ex)
            {
                output.Message = "Someting went wrong.";
                output.StatusCode = 500;
                Logger.Error("Get Notes" + ex.StackTrace);
            }
            return output;
        }

        public async Task<GetNotesList> GetAll(GetNoteListInput getNoteListInput)
        {
            GetNotesList output = new GetNotesList();
            try
            {
                if (getNoteListInput.UserId != Guid.Empty || getNoteListInput.AppointmentId != Guid.Empty)
                {

                    var list = _userNotesRepository.GetAll().ToList();
                    //output.Count = list.Count;
                    //if (Convert.ToInt32(getNoteListInput.limit) > 0 && Convert.ToInt32(getNoteListInput.page) > 0 && list.Count > 0) {
                    //    list = list.Skip((Convert.ToInt32(getNoteListInput.page) - 1) * Convert.ToInt32(getNoteListInput.limit)).Take(Convert.ToInt32(getNoteListInput.limit)).ToList();
                    //}

                    if (getNoteListInput.UserId != Guid.Empty && getNoteListInput.AppointmentId != Guid.Empty)
                    {
                        var appointment = await _appoinmentRepository.FirstOrDefaultAsync(x => x.Id == getNoteListInput.AppointmentId && x.IsBooked == 1);
                        var user = _userRepository.GetAllIncluding(x => x.Roles).FirstOrDefault(x => x.UniqueUserId == getNoteListInput.UserId && x.IsActive == true);
                        var appointmentUser = _userRepository.GetAllIncluding(x => x.Roles).FirstOrDefault(x => x.UniqueUserId == appointment.UserId && x.IsActive == true);
                        var roles = await _userManager.GetRolesAsync(user);
                        if (appointment != null)
                        {

                            if (roles.Contains("Patient"))
                            {
                                output.Items = list.Where(x => x.UserId == getNoteListInput.UserId && x.AppointmentId == getNoteListInput.AppointmentId && x.IsActive == true).Select(x => new GetNotesOutput
                                {
                                    NoteId = x.Id,
                                    Notes = x.Notes,
                                    NoteDate = Convert.ToDateTime(x.CreatedOn).ToString("MMM dd yyyy hh:mm tt"),
                                    CreatedOn = Convert.ToDateTime(x.CreatedOn),
                                    AppointmentId = x.AppointmentId
                                }).OrderByDescending(x => x.CreatedOn).ToList();
                            }
                            else if (appointmentUser.UserType == "MedicalLegal" || appointmentUser.UserType == "Insurance")
                            {
                                output.Items = list.Where(x => x.AppointmentId == getNoteListInput.AppointmentId && x.IsActive == true && x.UserId == user.UniqueUserId).Select(x => new GetNotesOutput
                                {
                                    NoteId = x.Id,
                                    Notes = x.Notes,
                                    NoteDate = Convert.ToDateTime(x.CreatedOn).ToString("MMM dd yyyy hh:mm tt"),
                                    CreatedOn = Convert.ToDateTime(x.CreatedOn),
                                    AppointmentId = x.AppointmentId
                                }).OrderByDescending(x => x.CreatedOn).ToList();
                            }
                            else
                            {
                                output.Items = list.Where(x => x.UserId == getNoteListInput.UserId && x.AppointmentId == getNoteListInput.AppointmentId && x.IsActive == true).Select(x => new GetNotesOutput
                                {
                                    NoteId = x.Id,
                                    Notes = x.Notes,
                                    NoteDate = Convert.ToDateTime(x.CreatedOn).ToString("MMM dd yyyy hh:mm tt"),
                                    CreatedOn = Convert.ToDateTime(x.CreatedOn),
                                    AppointmentId = x.AppointmentId
                                }).OrderByDescending(x => x.CreatedOn).ToList();
                            }

                            output.Count = output.Items.Count;
                            if (Convert.ToInt32(getNoteListInput.limit) > 0 && Convert.ToInt32(getNoteListInput.page) > 0 && output.Items.Count > 0)
                            {
                                output.Items = output.Items.Skip((Convert.ToInt32(getNoteListInput.page) - 1) * Convert.ToInt32(getNoteListInput.limit)).Take(Convert.ToInt32(getNoteListInput.limit)).ToList();
                            }

                            output.Message = "Get notes successfully.";
                            output.StatusCode = 200;
                        }
                        else
                        {
                            output.Message = "No record found.";
                            output.StatusCode = 401;
                        }
                    }
                    else if (getNoteListInput.UserId != Guid.Empty)
                    {
                        output.Items = list.Where(x => x.UserId == getNoteListInput.UserId && x.IsActive == true).Select(x => new GetNotesOutput
                        {
                            NoteId = x.Id,
                            Notes = x.Notes,
                            NoteDate = Convert.ToDateTime(x.CreatedOn).ToString("MMM dd yyyy hh:mm tt"),
                            CreatedOn = Convert.ToDateTime(x.CreatedOn),
                            AppointmentId = x.AppointmentId
                        }).OrderByDescending(x => x.CreatedOn).ToList();

                        output.Count = output.Items.Count;
                        if (Convert.ToInt32(getNoteListInput.limit) > 0 && Convert.ToInt32(getNoteListInput.page) > 0 && output.Items.Count > 0)
                        {
                            output.Items = output.Items.Skip((Convert.ToInt32(getNoteListInput.page) - 1) * Convert.ToInt32(getNoteListInput.limit)).Take(Convert.ToInt32(getNoteListInput.limit)).ToList();
                        }
                        output.Message = "Get notes successfully.";
                        output.StatusCode = 200;
                    }
                    else
                    {
                        output.Message = "Bad request.";
                        output.StatusCode = 401;
                    }
                }
                else
                {
                    output.Message = "Bad request.";
                    output.StatusCode = 401;
                }

            }
            catch (Exception ex)
            {
                output.Message = "Someting went wrong.";
                output.StatusCode = 500;
                Logger.Error("Update Notes" + ex.StackTrace);
            }
            return output;
        }

        public async Task<UserNotesOutput> GetNotesForSamvaad(Guid AppointmentId, Guid UserId)
        {
            UserNotesOutput output = new UserNotesOutput();
            try
            {
                if (UserId != Guid.Empty || AppointmentId != Guid.Empty)
                {
                    var notes = await _userNotesRepository.FirstOrDefaultAsync(x => x.UserId == UserId && x.AppointmentId == AppointmentId && x.IsActive == true && x.IsSamvaad == true);
                    if (notes != null)
                    {
                        output.NotesId = notes.Id;
                        output.Notes = notes.Notes;
                        output.Message = " Get Note details successfully.";
                        output.StatusCode = 200;
                    }
                    else
                    {
                        output.Message = "No record found.";
                        output.StatusCode = 401;
                    }
                }
                else if (UserId == Guid.Empty)
                {
                    var notes = await _userNotesRepository.FirstOrDefaultAsync(x => x.AppointmentId == AppointmentId && x.IsActive == true && x.IsSamvaad == true);
                    if (notes != null)
                    {
                        output.NotesId = notes.Id;
                        output.Notes = notes.Notes;
                        output.Message = " Get Note details successfully.";
                        output.StatusCode = 200;
                    }
                    else
                    {
                        output.Message = "No record found.";
                        output.StatusCode = 401;
                    }
                }
                else
                {
                    output.Message = "Bad request.";
                    output.StatusCode = 401;
                }
            }
            catch (Exception ex)
            {
                output.Message = "Someting went wrong.";
                output.StatusCode = 500;
                Logger.Error("Get Notes" + ex.StackTrace);
            }

            return output;
        }

        public async Task<UserNotesOutput> Update(UserNotesInput input)
        {
            UserNotesOutput output = new UserNotesOutput();
            try
            {
                if (input.UserId != null && input.UserId != Guid.Empty && !string.IsNullOrEmpty(input.Notes) && !string.IsNullOrEmpty(input.NotesId))
                {
                    Guid Id = new Guid(input.NotesId);
                    var notes = await _userNotesRepository.GetAsync(Id);
                    if (notes != null)
                    {
                        notes.UserId = input.UserId;
                        notes.Notes = input.Notes;
                        notes.UpdatedBy = input.UserId.ToString();
                        notes.UpdatedOn = DateTime.UtcNow;

                        if (input.AppointmentId != null && input.AppointmentId != Guid.Empty)
                        {
                            notes.AppointmentId = input.AppointmentId;
                        }

                        await _userNotesRepository.UpdateAsync(notes);

                        output.Message = "Notes updated successfully.";
                        output.StatusCode = 200;
                    }
                    else
                    {
                        output.Message = "No record found.";
                        output.StatusCode = 401;
                    }
                }
                else
                {
                    output.Message = "Bad request.";
                    output.StatusCode = 401;
                }

            }
            catch (Exception ex)
            {
                output.Message = "Someting went wrong.";
                output.StatusCode = 500;
                Logger.Error("Update Notes" + ex.StackTrace);
            }
            return output;
        }
    }
}
