using Application.DTO;
using Application.DTO.Create;
using Application.DTO.Pagintion;
using Application.DTO.Read;
using Application.DTO.Update;
using Application.Interface.Repo;
using Application.Interface.Service;
using AutoMapper;
using Domain.Model;

namespace Application.Service
{
    public class TopicService(ITopicRepo topicRepo, IMapper mapper) : BaseService
    <Topic, TopicRDTO, TopicCDTO, TopicUDTO>(topicRepo, mapper), ITopicService
    {
    }
}
