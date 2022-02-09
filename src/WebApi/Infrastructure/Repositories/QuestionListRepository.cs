﻿using Application.Repositories;
using Application.UseCases.QuestionList.GetQuestionList;
using AutoMapper;
using Domain.Models;
using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories;

public class QuestionListRepository : GenericRepository<QuestionListModel, QuestionList>, IQuestionListRepository
{
    public QuestionListRepository(MyDbContext dbContext, IMapper mapper) : base(dbContext, mapper)
    {
    }

    public async Task<bool> AddQuestionsToList(QuestionListModel questionListModel, IEnumerable<int> interviewQuestionIds)
    {
        var questionList = _mapper.Map<QuestionList>(questionListModel);

        questionList.UpdatedAt = DateTime.Now;

        foreach (int id in interviewQuestionIds)
        {
            var question = new InterviewQuestion { Id = id };
            questionList.InterviewQuestions.Add(question);
            DbContext.Entry(question).State = EntityState.Unchanged;
        }

        try
        {
            DbContext.QuestionLists.Update(questionList);
            await DbContext.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            return false;
        }
    }

    public async Task<IEnumerable<QuestionListModel>> Get(GetQuestionListInput input)
    {
        IQueryable<QuestionList> questionLists = DbContext.QuestionLists.Include(ql => ql.InterviewQuestions);

        if (input.Text != null)
        {
            questionLists = questionLists.Where(ql => ql.Title.Contains(input.Text) || ql.Description.Contains(input.Text));
        }

        if (input.Categories != null && input.Categories.Any())
        {
            foreach (string category in input.Categories)
            {
                questionLists = questionLists.Where(ql => ql.InterviewQuestions.Any(iq => iq.Category == category));
            }
        }

        var result = await questionLists.ToListAsync();

        return _mapper.Map<IEnumerable<QuestionListModel>>(result);
    }
}
