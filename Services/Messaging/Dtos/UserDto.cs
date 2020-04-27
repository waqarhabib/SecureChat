﻿using System.ComponentModel.DataAnnotations;
using Helpers.Mapping;
using Helpers.Specifications.Attributes;

namespace Messaging.Dtos
{
    public class UserDto
    {
        public ProfileDto Profile { get; set; }

        public string Id { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }

    }
}
