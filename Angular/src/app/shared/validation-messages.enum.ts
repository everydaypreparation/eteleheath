export enum ValidationMessages {
    NoResultsFound = "No result based on your search criteria",
    NoDoctorsResultsFound = "No Doctor is available as per your search criteria, please try after some time. Exit",
    enterFirstName = "Please enter First Name",
    firstNameNotString = "First Name should only contain alphabets",
    firstNameLength = "First Name should be 2 to 40 characters long",
    enterLastName = "Please enter Last Name",
    lastNameNotString = "Last Name should only contains alphabets",
    lastNameLength = "Last Name should be 2 to 40 characters long",
    enterEmail = "Please enter Email Address",
    emailNotString = "Email Address should not contain special characters",
    emailNotValid = "Please enter valid Email Address",
    emailLength = "Email should be 2 to 128 characters long",
    enterAffiliationPrimary = "Please enter Affiliation Primary",
    enterOncologySpecialty = "Please enter Oncology Specialty",
    enterOncologySubSpecialty = "Please enter Oncology Sub Specialty",
    enterHospitalAffiliation = "Please enter Hospital Affiliation",
    enterTimezone = "Please enter Timezone",

    usernameNotAlphanumeric = "Username should only contain alphabets and numbers",
    enterDoctorFirstName = "Please enter Doctor's First Name",
    enterDoctorLastName = "Please enter Doctor's Last Name",
    enterDoctorEmail = "Please enter Doctor's Email Address",
    signatureRequired = "Signature is required",

    //Sign-in
    enterUsernameOrEmail = "Please enter Username or Email Address",
    enterPassword = "Please enter Password",
    passwordLength = "Password should be 6 to 40 characters long",

    //Appointment booking
    requiredField = "This field is required",
    slotNotAvailable = "The slot you selected is no longer available. Please select another slot to continue.",

    //Finding consultant
    selectSearchCriteria = "Select a search criteria for the name",

    //Create Availability slots
    allFieldsRequired = "All fields are required",

    //Avalability slot edit/delete confirmation
    editAvalabilitySlotConfirmation = "Are you sure you want to edit this availability slot?",
    deleteAvalabilitySlotConfirmation = "Are you sure you want to delete this availability slot?",
    confirmCancelAppointment = "Are you sure you want to cancel this appointment?",
    confirmRescheduleAppointment = "Are you sure you want to reschedule this appointment?",
    confirmAppointmentBook = "Are you sure you want to book this appointment?",
    //Delete user confirmation
    deleteUserConfirmation = "Are you sure you want to delete this user?",

    //Delete request doctor confirmation
    deleteRequestDoctorConfirmation = "Are you sure you want to delete this record?",

    //Confirm logout
    confirmLogout = "Are you sure you want to logout?",

    //password match
    passwordMatch = "Confirm password must be match to new password",

    //Delete confirmation
    deleteMessagesConfirmation = "Are you sure you want to delete messages?",
    //Document edit/delete confirmation
    confirmDocumentEdit = "Are you sure you want to edit this document?",
    confirmDocumentDelete = "Are you sure you want to delete this document?",

    noFileSelected = "Please select a document",

    familyDoctorNotRegistered = "Note: These details do not match with any registered family doctor",

    //Delete note confirmation
    deleteNoteConfirmation = "Are you sure you want to delete this note?",
    addNoteConfirmation = "Are you sure you want to add this note?",

    enterTitle = "Please enter Title",
    consultationType = "Please enter Consultation Type",
    enterDescription = "Please enter Description",

    deleteConsentFormConfirmation = "Are you sure you want to delete this consent form?",

    //Payment details
    enterNameOnCard = "Please enter Name",
    nameOnCardNotString = "Name should only contain alphabets",
    nameOnCardLength = "Name should be 2 to 40 characters long",
    enterCardNumber = "Please enter Card Number",
    enterValidCardNumber = "Please enter valid Card Number",
    enterCVV = "Please enter CVV",
    enterValidCVV = "Please enter valid CVV",
    enterCardExpirationDetails = "Please enter Card Expiration Details",
    enterCardValidExpirationDetails = "Please enter valid Card Expiration Details",
    enterValidCouponCode = "Please enter valid Coupon Code",
    couponCodeExpired = "This Coupon Code has expired",

    //onboard users
    onboardUserConfirmation = "Are you sure you want to onboard this user?",
    deOnboardUserConfirmation = "Are you sure you want to reject this user from onboarding?",

    //Navigation
    completeProfileInfo = "Please fill all the mandatory details",
    navigationNotAllowed = "You can not view that page",
    navigationNotAllowedForPatient = "You have to book some appointment to see your dashboard",
    coupanMessage = " Code is successfully applied.",

    enterNumberOfPages = "Please enter total number of pages",

     //Delete request doctor confirmation
     archiveRecordConfirmation = "Are you sure you want to archive this record?",

     //paymentMessage
     paymentMessage = "Note: You have insufficient balance in your account, Please contact with ETeleHealth admin for balance.",
      //password match
      passwordMatches = "Passwords do not match.",

    //Delete request doctor confirmation
    deleteNotificationConfirmation = "Are you sure you want to delete this record?",

    //Delete request doctor confirmation
    roleConfirmation = "Please select atleast one role",

    patientNotRegistered = "Note: These details do not match with any registered patient",

    documentNotFound = "Document is not available",

    slotOverlappingErrorMsg = "Slots are overlapping, please correct them before submitting.",
    noDiagnosticSelected = "Please select a diagnostic",
    noRoleSelected = "Please select a role",

    enterRelationshipWithPatient = "Please enter relationship with patient",
    relationshipWithPatientNotString = "Relationship with patient should only contains alphabets",
    relationshipWithPatientLength = "Relationship with patient should be 2 to 80 characters long",
    selectAllUsers = "Are you sure you want to send the messages to all users?",
  
    //Delete feedback confirmation
    deleteFeedbackConfirmation = "Are you sure you want to delete this feedback?",
}
