import { IMdSetting } from "./IMdSetting";

export class MdSetting implements IMdSetting{

  public constructor(init?: Partial<MdSetting>) {
    Object.assign(this, init);
  }
    name: string;
    intValue?: number;
    stringValue?: string;
    dateTimeValue?: Date;

}
