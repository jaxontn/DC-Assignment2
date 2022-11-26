using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using DataBaseWebAPI.Models;

namespace DataBaseWebAPI.Controllers
{
    public class CentresController : ApiController
    {
        private CentredbEntities db = new CentredbEntities();

        // GET: api/Centres
        public IQueryable<Centre> GetCentres()
        {
            return db.Centres;
        }

        // GET: api/Centres/5
        [ResponseType(typeof(Centre))]
        public IHttpActionResult GetCentre(int id)
        {
            Centre centre = db.Centres.Find(id);
            if (centre == null)
            {
                return NotFound();
            }

            return Ok(centre);
        }

        // PUT: api/Centres/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutCentre(int id, Centre centre)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != centre.Id)
            {
                return BadRequest();
            }

            db.Entry(centre).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CentreExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Centres
        [ResponseType(typeof(Centre))]
        public IHttpActionResult PostCentre(Centre centre)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Centres.Add(centre);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                if (CentreExists(centre.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = centre.Id }, centre);
        }

        // DELETE: api/Centres/5
        [ResponseType(typeof(Centre))]
        public IHttpActionResult DeleteCentre(int id)
        {
            Centre centre = db.Centres.Find(id);
            if (centre == null)
            {
                return NotFound();
            }

            db.Centres.Remove(centre);
            db.SaveChanges();

            return Ok(centre);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool CentreExists(int id)
        {
            return db.Centres.Count(e => e.Id == id) > 0;
        }
    }
}