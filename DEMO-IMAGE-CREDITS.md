# Demo listing photography — credits

The four demo listings seeded into the development database use openly-licensed
photographs of the actual townships from [Wikimedia Commons](https://commons.wikimedia.org),
rather than generic property stock. Stock imagery of suburban homes was rejected because
pairing it with a R1 600/month backroom in Khayelitsha misrepresents the listing.

**These are demo images only.** They depict the area, not the specific dwelling — real
listings must use photographs supplied by the owner. Delete these listings before any
production deployment.

Every file below is CC BY or CC BY-SA, so **attribution is required wherever they are
displayed**. If these images are ever shown outside local development, surface the
author and licence in the UI, or replace them.

| Listing | File | Author | Licence |
| --- | --- | --- | --- |
| Orlando West | [Orlando West, Soweto, 1804, South Africa - panoramio (1).jpg](https://commons.wikimedia.org/wiki/File:Orlando_West,_Soweto,_1804,_South_Africa_-_panoramio_(1).jpg) | L-BBE | CC BY 3.0 |
| Orlando West | [House interior - Soweto (4612448514).jpg](https://commons.wikimedia.org/wiki/File:House_interior_-_Soweto_(4612448514).jpg) | Jorge Láscar | CC BY 2.0 |
| Orlando West | [Klipspruit 298-Iq, Soweto, 1809, South Africa - panoramio.jpg](https://commons.wikimedia.org/wiki/File:Klipspruit_298-Iq,_Soweto,_1809,_South_Africa_-_panoramio.jpg) | L-BBE | CC BY 3.0 |
| Diepkloof | [Klipspruit 298-Iq, Soweto, 1809, South Africa - panoramio (4).jpg](https://commons.wikimedia.org/wiki/File:Klipspruit_298-Iq,_Soweto,_1809,_South_Africa_-_panoramio_(4).jpg) | L-BBE | CC BY 3.0 |
| Diepkloof | [Soweto township.jpg](https://commons.wikimedia.org/wiki/File:Soweto_township.jpg) | Matt-80 | CC BY 2.0 |
| Tembisa | [Tembisa township, Gauteng, South Africa.jpg](https://commons.wikimedia.org/wiki/File:Tembisa_township,_Gauteng,_South_Africa.jpg) | Joperd | CC BY 2.0 |
| Tembisa | [Vusumuzi Informal Settlement, Tembisa.jpg](https://commons.wikimedia.org/wiki/File:Vusumuzi_Informal_Settlement,_Tembisa.jpg) | Basetsana M | CC BY-SA 4.0 |
| Khayelitsha | [Khayelitsha Site-C Cash-Store.JPG](https://commons.wikimedia.org/wiki/File:Khayelitsha_Site-C_Cash-Store.JPG) | FreddieA | CC BY-SA 3.0 |
| Khayelitsha | [Khayelitsha, Baden Powell Drive (South Africa).jpg](https://commons.wikimedia.org/wiki/File:Khayelitsha,_Baden_Powell_Drive_(South_Africa).jpg) | Olga Ernst | CC BY-SA 4.0 |
| Khayelitsha | [Khayelitsha Lookout-Hill Ilitha-Park.jpg](https://commons.wikimedia.org/wiki/File:Khayelitsha_Lookout-Hill_Ilitha-Park.jpg) | FreddieA | CC BY-SA 3.0 |

Seeded via the public API (register → login → `POST /properties` → `POST /properties/{id}/images`)
as `demo.owner@ekasiproperty.co.za`. Re-running the seed script deletes that owner's existing
listings first, so it is safe to repeat.
