﻿using Domain.Entities;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public class InterviewQuestion : EntityBase, IEntity
{
    public int Id { get; set; }

    public string Title { get; set; }

    public int? Difficulty { get; set; }

    public string Category { get; set; }

    public string Content { get; set; }

    public ICollection<QuestionListInterviewQuestion> QuestionListInterviewQuestions { get; set; }
}
