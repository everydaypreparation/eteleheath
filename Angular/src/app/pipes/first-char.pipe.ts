import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'firstChar'
})
export class FirstCharPipe implements PipeTransform {

  transform(value: string, ...args: unknown[]): unknown {
    value = value.charAt(0);
    return value.toUpperCase();
  }

}
