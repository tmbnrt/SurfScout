using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurfScout.Services
{
    class SessionPlannerService
    {
        // TODO: Send request to API to delete user (by id) from the planned session
        // in backend: delete session if no participants left

        // TODO: Send put request to add user to an existing session
        // in backend: if session has been removed in the meantime, create new session

        // TODO: Send get request to get all planned sessions for the user and the friends of the user (Update after every request!)
        // but update PlannedSessionStore only for the current sport mode
    }
}
