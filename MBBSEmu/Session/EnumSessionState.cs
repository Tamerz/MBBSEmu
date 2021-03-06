﻿namespace MBBSEmu.Session
{
    public enum EnumSessionState
    {
        Negotiating,

        /// <summary>
        ///     Initial State for all Sessions
        /// </summary>
        Unauthenticated,

        /// <summary>
        ///     Displaying the Username Prompt for Input
        /// </summary>
        LoginUsernameDisplay,

        /// <summary>
        ///     User is being prompted for their Username
        /// </summary>
        LoginUsernameInput,

        /// <summary>
        ///     Displaying the Password Prompt for Input
        /// </summary>
        LoginPasswordDisplay,

        /// <summary>
        ///     User is being prompted for their Password
        /// </summary>
        LoginPasswordInput,

        /// <summary>
        ///     User is currently seeing Login Routines from Modules
        /// </summary>
        LoginRoutines,
        
        /// <summary>
        ///     User is at the Main Menu
        /// </summary>
        MainMenuDisplay,

        MainMenuInput,

        /// <summary>
        ///     User is Signing up, prompted for Username
        /// </summary>
        SignupUsernameDisplay,

        SignupUsernameInput,

        /// <summary>
        ///     User is Signing up, prompted for Password
        /// </summary>
        SignupPasswordDisplay,

        SignupPasswordInput,

        SignupPasswordConfirmDisplay,
        SignupPasswordConfirmInput,

        SignupEmailDisplay,
        SignupEmailInput,

        /// <summary>
        ///     User is Signing up, prompted for Password Confirmation
        /// </summary>
        SignupPasswordConfirm,

        /// <summary>
        ///     User is Entering Module (initial routines running)
        /// </summary>
        EnteringModule,

        /// <summary>
        ///     User is In Module (waiting for input)
        /// </summary>
        InModule,

        /// <summary>
        ///     User is Existing Module
        /// </summary>
        ExitingModule,

        ConfirmLogoffDisplay,

        ConfirmLogoffInput,

        /// <summary>
        ///     User is in the process of Logging Off
        /// </summary>
        LoggingOffDisplay,

        LoggingOffProcessing,

        /// <summary>
        ///     User is logged off, dispose of Session
        /// </summary>
        LoggedOff,

        EnteringFullScreenDisplay,

        InFullScreenDisplay,

        EnteringFullScreenEditor,

        InFullScreenEditor,

        ExitingFullScreenDisplay
    }
}