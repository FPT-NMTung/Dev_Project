﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Project.Filters {
    public class CheckSession : ActionFilterAttribute {
        public override void OnActionExecuting(ActionExecutingContext filterContext) {
            var currentSession = HttpContext.Current.Session;

            if ( currentSession["email"] == null ||
                 currentSession["user_id"] == null ||
                 currentSession["name"] == null ) {
                filterContext.Result = new RedirectResult( "/login" );
            }
        }
    }
}