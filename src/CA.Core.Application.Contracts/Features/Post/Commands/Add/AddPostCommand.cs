﻿using CA.Core.Application.Contracts.Response;
using MediatR;

namespace CA.Core.Application.Contracts.Features.Post.Commands.Add
{
    public class AddPostCommand : IRequest<Response<int>>
    {
        public string Title { get; set; }

        public string Slug { get; set; }
        public string Summary { get; set; }
        public string Content { get; set; }
    }
}
