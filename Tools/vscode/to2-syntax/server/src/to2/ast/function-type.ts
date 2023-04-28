import { ModuleContext } from "./context";
import { RealizedType, TO2Type } from "./to2-type";

export class FunctionType implements RealizedType {
  public name: string;
  public description: string;
  public localName: string;

  constructor(
    public readonly isAsync: boolean,
    public readonly parameterTypes: TO2Type[],
    public readonly returnType: TO2Type
  ) {
    this.name = `${isAsync ? "" : "sync "}fn(${parameterTypes.join(
      ", "
    )}) -> ${returnType}`;
    this.description = "";
    this.localName = this.name;
  }

  public isAssignableFrom(otherType: RealizedType): boolean {
    return this.name === otherType.name;
  }

  public realizedType(context: ModuleContext): RealizedType {
    return this;
  }

  public findSuffixOperator(): RealizedType | undefined {
    return undefined;
  }

  public findPrefixOperator(): RealizedType | undefined {
    return undefined;
  }
}