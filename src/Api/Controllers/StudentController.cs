﻿using System;
using System.Collections.Generic;
using System.Linq;
using Logic.Dtos;
using CSharpFunctionalExtensions;
using FluentNHibernate.Testing.Values;
using Logic.AppServices;
using Logic.Students;
using Logic.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/students")]
    public sealed class StudentController : BaseController
    {
        private readonly Messages _messages;

        public StudentController( Messages messages)
            
        {
            _messages = messages;
        }

        [HttpGet]
        public IActionResult GetList(string enrolled, int? number) 
        {
            List<StudentDto> students =  _messages.Dispatch( new GetListQuery(enrolled, number));
            return Ok(students);
        }
        

        [HttpPost]
        public IActionResult Register([FromBody] NewStudentDto dto) 
        {
            Result result = _messages.Dispatch(
                new RegisterCommand(
                dto.Name , dto.Email , dto.Course1 , dto.Course1Grade  , dto.Course2 , dto.Course2Grade)
            );
            return FromResult(result);
        }

        [HttpDelete("{id}")] 
        public IActionResult Unregister(long id)
        {
            Result result = _messages.Dispatch(new UnregisterCommand(id));
            return FromResult(result);
        }
 
        [HttpPost("{id}/enrollments")] 
        public IActionResult Enroll(long id ,[FromBody] StudentEnrollmentDto dto )
        {
          Result result = _messages.Dispatch(new EnrollCommand(id , dto.Course , dto.Grade));

          return FromResult(result);
        }
        [HttpPost("{id}/enrollments/{enrollmentNumber}/deletion")]
        public IActionResult DisEnroll(long id ,int enrollmentNumber , 
            [FromBody] StudentDisEnrollDto dto ) 
        {
            Result result = _messages.Dispatch(new DisEnrollCommand(id , enrollmentNumber , dto.Comment));

            return FromResult(result);
        }
        
        [HttpPost("{id}/enrollments/{enrollmentNumber}")]
        public IActionResult TransferStudent(long id , int enrollmentNumber,
            [FromBody] StudentTransfertDto dto )
        {
            Result result = _messages.Dispatch(new TransferCommand(id , enrollmentNumber , dto.Course , dto.Grade));
            return FromResult(result);
        }
        
        [HttpPost("{id}")]
        public IActionResult EditPersonalInfo(long id ,[FromBody] StudentPersonalInfoDto dto ) //Command
        {
            // don`t send Icommand to the controller method  as parameter
            // but send a dto to the controller then map it to the command 
            // to tackle complexity and avoid coupling
            var command = new EditPersonalInfoCommand(dto.Email, dto.Name, id);
    
           Result result =  _messages.Dispatch(command);
           return FromResult(result);
        }
    }
}
