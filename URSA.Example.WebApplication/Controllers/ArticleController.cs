﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using RomanticWeb;
using RomanticWeb.Entities;
using URSA.Example.WebApplication.Data;
using URSA.Web;
using URSA.Web.Http.Description.Entities;
using URSA.Web.Http.Description.Mapping;
using URSA.Web.Mapping;

namespace URSA.Example.WebApplication.Controllers
{
    /// <summary>Provides a basic article handling.</summary>
    public class ArticleController : IWriteController<IArticle, Guid>, IHypermediaDrivenController<ArticleController, IArticle>
    {
        private readonly IList<IArticle> _repository;

        private readonly IEntityContext _entityContext;

        /// <summary>Initializes a new instance of the <see cref="ArticleController"/> class.</summary>
        /// <param name="entityContext">The entity context.</param>
        public ArticleController(IEntityContext entityContext)
        {
            if (entityContext == null)
            {
                throw new ArgumentNullException("entityContext");
            }

            _repository = new List<IArticle>();
            foreach (var article in (_entityContext = entityContext).AsQueryable<IArticle>())
            {
                _repository.Add(article);
            }
        }

        /// <inheritdoc />
        public IResponseInfo Response { get; set; }

        /// <summary>Gets all articles.</summary>
        /// <param name="totalItems">Number of total items in the collection if <paramref name="skip" /> and <paramref name="take" /> are used.</param>
        /// <param name="skip">Skips top <paramref name="skip" /> elements of the collection.</param>
        /// <param name="take">Takes top <paramref name="take" /> elements of the collection. Use 0 for all of the entities.</param>
        /// <param name="filter">Expression to be used for entity filtering.</param>
        /// <returns>Collection of entities.</returns>
        public IEnumerable<IArticle> List(
            out int totalItems,
            [LinqServerBehavior(LinqOperations.Skip), FromQueryString("{?$skip}")] int skip = 0,
            [LinqServerBehavior(LinqOperations.Take), FromQueryString("{?$top}")] int take = 0,
            [LinqServerBehavior(LinqOperations.Filter), FromQueryString("{?$filter}")] Expression<Func<IArticle, bool>> filter = null)
        {
            totalItems = _repository.Count;
            IEnumerable<IArticle> result = _repository;
            if (skip > 0)
            {
                result = result.Skip(skip);
            }

            if (take > 0)
            {
                result = result.Take(take);
            }

            if (filter != null)
            {
                result = result.Where(entity => filter.Compile()(entity));
            }

            return result;
        }

        /// <summary>Gets the article with identifier of <paramref name="id" />.</summary>
        /// <param name="id">Identifier of the article to retrieve.</param>
        /// <returns>Instance of the <see cref="IArticle" /> if matching <paramref name="id" />; otherwise <b>null</b>.</returns>
        public IArticle Get(Guid id)
        {
            return (from entity in _repository where entity.Key == id select entity).FirstOrDefault();
        }

        /// <summary>Creates the specified article.</summary>
        /// <param name="article">The article.</param>
        /// <returns>Identifier of newly created article.</returns>
        public Guid Create(IArticle article)
        {
            article.Key = Guid.NewGuid();
            article.Context.Commit();
            article = article.Rename(new EntityId(article.Id.Uri + (article.Id.Uri.AbsoluteUri.EndsWith("/") ? String.Empty : "/") + article.Key));
            _repository.Add(article);
            return article.Key;
        }

        /// <summary>Updates the specified article.</summary>
        /// <param name="id">The identifier of the article to be updated.</param>
        /// <param name="article">The article.</param>
        public void Update(Guid id, IArticle article)
        {
            (_repository[GetIndex(id)] = article).Key = id;
        }

        /// <summary>Deletes an article.</summary>
        /// <param name="id">Identifier of the article to be deleted.</param>
        public void Delete(Guid id)
        {
            var article = _repository[GetIndex(id)];
            _repository.Remove(article);
            _entityContext.Delete(article.Id);
            _entityContext.Commit();
        }

        private int GetIndex(Guid id)
        {
            int index = -1;
            _repository.Where((entity, entityIndex) => (entity.Key == id) && ((index = entityIndex) != -1)).FirstOrDefault();
            if (index == -1)
            {
                throw new NotFoundException(String.Format("Article with id of '{0}' does not exist.", id));
            }

            return index;
        }
    }
}