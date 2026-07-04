import { readFile, writeFile } from 'fs';
import type { Ports } from '../types/port';

// Load the JSON data
readFile('ports.json', 'utf8', (err, data) => {
  if (err) throw err;

  // Parse JSON data
  const ports = JSON.parse(data) as Ports;

  // Update each port's city with its name
  Object.keys(ports).forEach(portCode => {
    ports[portCode].city = ports[portCode].name;
    // console.log(ports[portCode].timezone)
    if (!ports[portCode].timezone) {
      console.log(portCode)
    }

  });

  // Convert the updated object back to JSON format
  const updatedData = JSON.stringify(ports, null, 2);

  // Write the updated data to the file
  writeFile('port.json', updatedData, 'utf8', (err) => {
    if (err) throw err;
    console.log('City fields have been updated with port names.');
  });
});
