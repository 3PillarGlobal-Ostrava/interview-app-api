﻿using Application.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.UseCases.QuestionSet.GetQuestionList;

public class GetQuestionSetUseCase : IGetQuestionSetUseCase
{
    private IOutputPort _outputPort;

    private readonly IQuestionSetRepository _questionListRepository;

    public GetQuestionSetUseCase(IQuestionSetRepository questionListRepository)
    {
        _questionListRepository = questionListRepository;
    }

    public async Task Execute(GetQuestionSetInput input)
    {
        if (input is null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        await GetQuestionListInternal(input);
    }

    public void SetOutputPort(IOutputPort outputPort) => _outputPort = outputPort;

    private async Task GetQuestionListInternal(GetQuestionSetInput input)
    {
        var questionSet = await _questionListRepository.GetById(input.Id);

        if (questionSet is null)
        {
            _outputPort.NotFound();
            return;
        }

        _outputPort.Ok(questionSet);
    }
}
