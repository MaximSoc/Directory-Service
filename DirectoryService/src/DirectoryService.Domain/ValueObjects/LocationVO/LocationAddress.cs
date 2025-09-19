﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.ValueObjects.LocationVO
{
    public record LocationAddress
    {
        public string Country { get; }
        public string Region { get; }
        public string City { get; }
        public string PostalCode { get; }
        public string Street { get; }
        public string ApartamentNumber { get; }

        public LocationAddress(
            string country,
            string region,
            string city,
            string postalCode,
            string street,
            string apartamentNumber)
        {
            Country = country;
            Region = region;
            City = city;
            PostalCode = postalCode;
            Street = street;
            ApartamentNumber = apartamentNumber;
        }

        public static Result<LocationAddress, string> Create(
            string country,
            string region,
            string city,
            string postalCode,
            string street,
            string apartamentNumber)
        {
            if (string.IsNullOrWhiteSpace(country)
                || string.IsNullOrWhiteSpace(region)
                || string.IsNullOrWhiteSpace(city)
                || string.IsNullOrWhiteSpace(postalCode)
                || string.IsNullOrWhiteSpace(street)
                || string.IsNullOrWhiteSpace(apartamentNumber))
            {
                return "All fields must be filled in";
            }

            return new LocationAddress(
                country,
                region,
                city,
                postalCode,
                street,
                apartamentNumber);
        }
    }
}
