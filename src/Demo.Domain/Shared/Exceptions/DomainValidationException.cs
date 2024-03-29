﻿using System;
using System.Collections.Generic;
using Demo.Domain.Shared.DomainEntity;
using Demo.Domain.Shared.Extensions;

namespace Demo.Domain.Shared.Exceptions;

public class DomainValidationException : Exception
{
    public DomainValidationException()
    {
    }

    public DomainValidationException(IList<ValidationMessage> validationMessages)
        : base(validationMessages.AsString())
    {
        ValidationMessages = validationMessages;
    }

    public DomainValidationException(IList<ValidationMessage> validationMessages, Exception innerException)
        : base(validationMessages.AsString(), innerException)
    {
        ValidationMessages = validationMessages;
    }

    public IEnumerable<ValidationMessage> ValidationMessages { get; }
}
