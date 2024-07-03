export function initialize()
{
    let blazorSchoolIndexedDb = indexedDB.open(DATABASE_NAME, CURRENT_VERSION);
    blazorSchoolIndexedDb.onupgradeneeded = function ()
    {
        let db = blazorSchoolIndexedDb.result;
        db.createObjectStore("Roms", { keyPath: "id" });
    }
}

let CURRENT_VERSION = 1;
let DATABASE_NAME = "QuadRando";

export function set(collectionName, value)
{
    let blazorSchoolIndexedDb = indexedDB.open(DATABASE_NAME, CURRENT_VERSION);

    blazorSchoolIndexedDb.onsuccess = function ()
    {
        let transaction = blazorSchoolIndexedDb.result.transaction(collectionName, "readwrite");
        let collection = transaction.objectStore(collectionName)
        collection.put(value);
    }
}

export async function get(collectionName, id)
{
    let request = new Promise((resolve) =>
    {
        let blazorSchoolIndexedDb = indexedDB.open(DATABASE_NAME, CURRENT_VERSION);
        blazorSchoolIndexedDb.onsuccess = function ()
        {
            let transaction = blazorSchoolIndexedDb.result.transaction(collectionName, "readonly");
            let collection = transaction.objectStore(collectionName);
            let result = collection.get(id);

            result.onsuccess = function (e)
            {
                resolve(result.result);
            }
        }
    });

    let result = await request;

    return result;
}

export async function exists(collectionName, id)
{
  let request = new Promise((resolve) => {
    let blazorSchoolIndexedDb = indexedDB.open(DATABASE_NAME, CURRENT_VERSION);
    blazorSchoolIndexedDb.onsuccess = function () {
      let transaction = blazorSchoolIndexedDb.result.transaction(collectionName, "readonly");
      let collection = transaction.objectStore(collectionName);
      let result = collection.count(id);

      result.onsuccess = function (e) {
        resolve(result.result !== undefined && result.result > 0);
      }
    }
  });

  let result = await request;
  return result;
}
