using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PrmServer.Controllers;
using PrmServer.DTOs;
using PrmServer.Entities;
using PrmServer.Services.Interfaces;
using Xunit;

namespace PrmServer.Tests.Controllers
{
    public class ProjectControllerTests
    {
        private readonly Mock<IProjectService> _projectServiceMock;
        private readonly ProjectController _controller;

        public ProjectControllerTests()
        {
            _projectServiceMock = new Mock<IProjectService>();
            _controller = new ProjectController(_projectServiceMock.Object);
        }

        [Fact]
        public async Task Create_ReturnsOk_WithValidProject()
        {
            var dto = new CreateProjectDto();
            var expectedProject = new Project { Id = 1, Name = "Test Project" };

            _projectServiceMock.Setup(s => s.CreateAsync(dto))
                .ReturnsAsync(expectedProject);

            var result = await _controller.Create(dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedProject, okResult.Value);
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_OnInvalidDates()
        {
            var dto = new CreateProjectDto();

            _projectServiceMock.Setup(s => s.CreateAsync(dto))
                .ThrowsAsync(new ArgumentException("Start date must be before end date."));

            var result = await _controller.Create(dto);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            // Verify structure returned (which is an anonymous object { error = ex.Message })
            var value = badRequestResult.Value;
            var propertyInfo = value.GetType().GetProperty("error");
            Assert.NotNull(propertyInfo);
            var errorMsg = propertyInfo.GetValue(value) as string;
            Assert.Equal("Start date must be before end date.", errorMsg);
        }

        [Fact]
        public async Task AddMilestone_ReturnsBadRequest_IfSPExceeded()
        {
            var projectId = 1;
            var dto = new AddMilestoneDto();

            _projectServiceMock.Setup(s => s.AddMilestoneAsync(projectId, dto))
                .ThrowsAsync(new InvalidOperationException("Milestone points cannot exceed total remaining"));

            var result = await _controller.AddMilestone(projectId, dto);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var value = badRequestResult.Value;
            var propertyInfo = value.GetType().GetProperty("message");
            Assert.NotNull(propertyInfo);
            var errorMsg = propertyInfo.GetValue(value) as string;
            Assert.Equal("Milestone points cannot exceed total remaining", errorMsg);
        }
    }
}
