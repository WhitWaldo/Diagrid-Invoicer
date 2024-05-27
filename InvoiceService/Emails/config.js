/** @type {import('@maizzle/framework').Config} */

/*
|-------------------------------------------------------------------------------
| Development config                      https://maizzle.com/docs/environments
|-------------------------------------------------------------------------------
|
| The exported object contains the default Maizzle settings for development.
| This is used when you run `maizzle build` or `maizzle serve` and it has
| the fastest build time, since most transformations are disabled.
|
*/

module.exports = {
    baseURL: {
      url: 'https://cdn.invoicer.whit.dev/',
      tags: ['img', 'source']
    },
    build: {
      templates: {
        source: 'src/templates',
        destination: {
          path: 'build_local',
        },
      },
    },
    company: {
      name: 'Whit Waldo',
      product: 'Invoicer',
      sender: 'me@whit.dev'
    },
    year: () => new Date().getFullYear()
  }
  