﻿using Application.UseCases.QuestionSet.GetQuestionSets;
using Domain.Models;
using Domain.Models.Views;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace WebApi.UseCases.v1.QuestionSets.GetQuestionSet;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class QuestionSetController : ControllerBase, IOutputPort
{
    private IActionResult _viewModel;

    private readonly IGetQuestionSetsUseCase _useCase;

    public QuestionSetController(IGetQuestionSetsUseCase useCase)
    {
        _useCase = useCase;
    }

    void IOutputPort.Invalid()
    {
        _viewModel = BadRequest();
    }

    void IOutputPort.Ok(IEnumerable<QuestionSetListItem> questionSet)
    {
        _viewModel = Ok(questionSet);
    }

    void IOutputPort.NotFound()
    {
        _viewModel = NotFound();
    }

    [HttpGet(Name = "GetQuestionSets")]
    [ProducesResponseType(typeof(IEnumerable<QuestionSetListItem>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> Get()
    {
        _useCase.SetOutputPort(this);

        var input = new GetQuestionSetsInput()
        {
            QueryString = "",
            Category = null
        };

        await _useCase.Execute(input);

        return _viewModel;
    }
}
