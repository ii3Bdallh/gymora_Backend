using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTO.Create;
using Application.DTO.Read;
using Application.DTO.Update;
using Domain.Model;

namespace Application.Interface.Service
{
    public interface ITopicService : IBaseService<Topic, TopicRDTO, TopicCDTO, TopicUDTO>
    {

    }
}