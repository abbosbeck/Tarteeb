﻿//=================================
// Copyright (c) Coalition of Good-Hearted Engineers
// Free to use to bring order in your workplace
//=================================

using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;
using Tarteeb.Api.Models;
using Tarteeb.Api.Models.Users.Exceptions;
using Xeptions;

namespace Tarteeb.Api.Services.Foundations.Users
{
    public partial class UserService
    {
        private delegate ValueTask<User> ReturningUserFunction();

        private async ValueTask<User> TryCatch(ReturningUserFunction returningUserFunction)
        {
            try
            {
                return await returningUserFunction();
            }
            catch (NullUserException nullUserException)
            {
                throw CreateAndLogValidationException(nullUserException);
            }
            catch (InvalidUserException invalidUserException)
            {
                throw CreateAndLogValidationException(invalidUserException);
            }
            catch(SqlException sqlException)
            {
                var failedUserStorageException =new FailedUserStorageException(sqlException);

                throw CreateAndLogCriticalDependencyException(failedUserStorageException);
            }
        }

        private UserDependencyException CreateAndLogCriticalDependencyException(Xeption exeption)
        {
            var userDependencyException =new UserDependencyException(exeption);
            this.loggingBroker.LogCritical(userDependencyException);

            return userDependencyException;
        }

        private UserValidationException CreateAndLogValidationException(Xeption exception)
        {
            var userValidationException =
                new UserValidationException(exception);

            this.loggingBroker.LogError(userValidationException);

            return userValidationException;
        }
    }
}
