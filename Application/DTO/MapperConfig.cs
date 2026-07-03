using Application.DTO.Create;
using Application.DTO.Read;
using Application.DTO.Update;
using AutoMapper;
using Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO
{
    public class MapperConfig : Profile
    {
        public MapperConfig()
        {
         

  

        
                


            CreateMap<Topic, TopicCDTO>().ReverseMap();
            CreateMap<Topic, TopicUDTO>()
                .ReverseMap();
          

            CreateMap<Topic, TopicRDTO>()
                .ReverseMap();


        }
    }
}